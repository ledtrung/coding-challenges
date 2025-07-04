### Elsa info:
- System design
- Number of users
- Number of concurrent users

https://claude.ai/chat/863c850a-4e81-4753-a9f5-9b1abf934f96


### Decisions
- Separated web socket services and quiz service, due to the big number of concurrent users.
- Only use web socket for score updates, dashboard updates ... http for other things
- Cache quiz


### Steps
- Ask about Elsa business/system design/technical stack
- Ask about Elsa number of users, estimated peak concurrent users
- 