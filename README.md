## Understanding Eventsourcing - The Book

This is a csharp implementation of the one accompanying the book  original repo: https://github.com/dilgerma/eventsourcing-book.

Key points:
- Domain model uses "deciders" as explained in the [Functional Event Sourcing Decider](https://thinkbeforecoding.com/post/2021/12/17/functional-event-sourcing-decider) article by Jeremie chassaing
  - Using deciders allows for the business logic to be written independently of whether the state is state-or-event stored
- Implements both a "state stored" and an "event stored" variant of the eventmodel.
  - When running the program as "state stored", it saves the latest state via entity framework
  - When running as "event sourced", the events get stored in eventStoreDb

