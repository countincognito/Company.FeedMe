# Company.FeedMe

This is a simple example of the "Feed Me" Pattern for event-driven microservices.

It requires a local installation of [Seq](https://getseq.net/) for logging, and a minimum of version 5.6 of the [Microsoft Azure Storage Emulator](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator).

In an event-driven architecture, microservices often store their own copies of all data records (even those owned by other microservices). The idea is that each microservice retains its own copy of all available data, thus ensuring that it can function autonomously. Changes to data records are replicated across microservices via events that are published whenever an individual microservice updates a given record - the change is percolated across all the other microservices until all of them eventually become consistent.

There is a risk in such a model: since the events may not always be guaranteed to be delivered (or properly assimilated at their destination), there is always the possibility of one or more microservices will end up with inconsistent data. The problem then becomes one of data correction.

The "Feed Me" Pattern works in the following way:

- Microservice 1 needs information about record A, but its data records are incomplete/out-of-date.
- Microservice 1 stops its current task, puts it on a queue to be processed again later (e.g. after 10 seconds) in the hope that its data records will be consistent by then.
- Microservice 1 publishes a message asking all the other microservices if they have the required information.
- Microservice 2 (perhaps the primary owner of the original data record) receives the message and publishes (or re-publishes) an event with the necessary information.
- Microservice 1 receives the event and updates its own data records.
- Microservice 1 pulls its original task back off the queue and can now process it successfully.

This solution has three microservice: [`UserManager`](https://github.com/countincognito/Company.FeedMe/blob/master/src/Company.Manager.User.Impl/UserManager.cs), [`OrganisationManager`](https://github.com/countincognito/Company.FeedMe/blob/master/src/Company.Manager.Organisation.Impl/OrganisationManager.cs), and [`AppointmentManager`](https://github.com/countincognito/Company.FeedMe/blob/master/src/Company.Manager.Appointment.Impl/AppointmentManager.cs). The `UserManager` and `OrganisationManager` simply create data records for users and organisations. The `AppointmentManager` creates appointments that require both a `userId` and an `organisationId`. However, it relies on receiving update events from the `UserManager` and the `OrganisationManager` in order to keep its own data records up-to-date.

The `UserManager` and `OrganisationManager` both publish update events whenever a new data record is created, in which case the data records of the `AppointmentManager` will always be consistent. However, if the `silent` flag is set to `true` whenever a new user or organisation is added then no update events will be published. In that instance, the `AppointmentManager` initiates the "Feed Me" Pattern in order to correct its own data records.

To see the pattern in action simply do the following:

- Start the **Azure Storage Emulator**.
- Run the **Test.InProc.RestApi** and go to the main **Swagger** page.
- Send a `POST` to `/api/appointments/add` with the following input:

```json
{
  "id": 5,
  "userId": 6,
  "organisationId": 7
}
```

- The response should return a 400 status as no such user or organisation exist yet in the data records of the `AppointmentManager`.
- Send a `POST` to `/api/appointments/find` with an `id` of 5 (the response should return a 400 status as no such appointment exists yet).
- Send a `POST` to `/api/users/add` with the following input:

```json
{
  "id": 6,
  "name": "user 6",
  "silent": true
}
```

- Send a `POST` to `/api/users/add` with the following input:

```json
{
  "id": 7,
  "name": "org 7",
  "silent": true
}
```

- Once again send a `POST` to `/api/appointments/find` with an `id` of 5.
	- If the response returns a 400 status that is fine - the `AppointmentManager` waits 10 seconds between each attempt to correct its data records.
	- Wait a few seconds and repost the request. The response should eventually return a 200 status with the following output:

```json
{
  "id": 5,
  "userId": 6,
  "organisationId": 7
}
```

A couple points to note:

- Once the **Azure Storage Emulator** and **Test.InProc.RestApi** are running, you can shuffle the order of the creation steps around as much as you want and the self-correction should always eventually work.
- If you wish, you can enhance the pattern by limiting the number of self-correction retries to a reasonable fixed number (e.g. 3) -  right now (for illustrative purposes) it will just keep trying indefinitely.
