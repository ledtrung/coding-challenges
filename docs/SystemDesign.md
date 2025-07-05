## System overview

```mermaid
graph TB
    %% Client Layer
    WEB[Client Application]
    
    %% Microservices
    subgraph "Microservices Layer"
        subgraph "Quiz API Service (Port 5001)"
            QA[Quiz API<br/>• Answer Processing<br/>• Score Calculation<br/>• Business Logic<br/>• Cache Management]
        end
        
        subgraph "WebSocket Service (Port 5002)"
            WS[WebSocket Hub<br/>• Real-time Updates<br/>• Connection Management<br/>• Event Broadcasting<br/>• SignalR]
        end
    end
    
    %% Database Layer
    subgraph "Database Layer"
        PG_MASTER[PostgreSQL Master<br/>• Quiz Data<br/>• User Scores<br/>• Answer Records<br/>• ACID Transactions]
    end
    
    %% Caching Layer
    subgraph "Caching Layer"
        subgraph "Redis Cluster"
            REDIS_CACHE[Redis Cache<br/>• Quiz Data Cache<br/>• Session Storage<br/>• Leaderboards]
            REDIS_PUBSUB[Redis Pub/Sub<br/>• Event Broadcasting<br/>• Service Communication<br/>• Real-time Events]
        end
        
        subgraph "Application Cache"
            MEM_CACHE[In-Memory Cache<br/>• Active Quiz Sessions<br/>• Frequent Data<br/>• 30min TTL]
        end
    end
    
    %% Connection Flows
    
    %% Client to WebSocket (Real-time)
    WEB -.->|"WebSocket<br/>SignalR Connection"| WS
    
    %% Load Balancer Routing
    WEB -->|"Route /api/*"| QA
    
    %% Inter-Service Communication
    QA -->|"Publish Events<br/>Score Updates"| REDIS_PUBSUB
    WS -->|"Subscribe Events<br/>Broadcast Updates"| REDIS_PUBSUB
    
    %% Caching Flows
    QA <-->|"Cache Quiz Data<br/>L1: Memory, L2: Redis"| MEM_CACHE
    QA <-->|"Cache Questions<br/>Answer Keys, Points"| REDIS_CACHE
    WS <-->|"Session Storage<br/>Connection State"| REDIS_CACHE
    
    %% Database Connections
    QA -->|"Write Operations<br/>Scores, Answers"| PG_MASTER
    
    
    %% Styling
    classDef clientClass fill:#e1f5fe,stroke:#01579b,stroke-width:2px
    classDef serviceClass fill:#f3e5f5,stroke:#4a148c,stroke-width:2px
    classDef cacheClass fill:#fff3e0,stroke:#e65100,stroke-width:2px
    classDef dbClass fill:#e8f5e8,stroke:#1b5e20,stroke-width:2px
    classDef infraClass fill:#fce4ec,stroke:#880e4f,stroke-width:2px
    
    class WEB clientClass
    class QA,WS serviceClass
    class REDIS_CACHE,REDIS_PUBSUB,MEM_CACHE cacheClass
    class PG_MASTER dbClass
```

### Conditions
Elsa has very high number of
- Total users: 15+ millions
- Users: 4+ millions/day
- Concurrent users, estimated to be
    - Normal peak: Around 25k-50k
    - Global peak: 75k-100k
    - Spikes: 150k+
The architecture I've chosen based on those numbers.

### Key decisions
- **Service Separation**: WebSocket service isolated to handle connection overhead without affecting API performance. Allow us to scale them separately.
- **Multi-Level Caching**: L1 (in-memory) + L2 (Redis) reduces database load by ~90%.
- **Redis Pub/Sub**: Enables real-time score updates across all connected clients. Simple to use. Consider using Kafka for higher throughput and better durability and guarantee delivery.
- **Partially adopt DDD**: Fully adopt DDD is not a good idea for Quiz application where (from my point of view) the logic is not complicate enough. But there are still a few things we can use:
    - Enhanced encapsulation and behavior-rich domain models.
    - Clear separation of concerns between application logic, domain logic and infrastructure logic.
    - (Almost) Always valid models

### Missing
#### Monitoring & Observability
Due to the time limit, I couldn't implement monitoring in the code, I will discuss about it in this session.

- **Health checks**: Since we are building a distributed system, making sure our dependencies are up and running is extremely crucial. We need to have health checks for our services.
- **Application/Infrastructure Metrics**: Knowing what our end-users are experiencing, how our infrastructure is working, what need to be fix ... is crucial.
- **Distributed tracing**: It's a must to have an overview of how a specific request is being served, across services/components.
- **Arlerting**: We need to know as soon as there is something bad happen to our Quiz application.


