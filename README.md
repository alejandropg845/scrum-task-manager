<div align="center">
  <a href="./README.esp.md">
    <img src="https://img.shields.io/badge/Leer_en_Espa%C3%B1ol-ES-red?style=for-the-badge" alt="Leer en Español">
  </a>
</div>

# Scrum Task Manager

![Angular](https://img.shields.io/badge/Angular-DD0031?style=for-the-badge&logo=angular&logoColor=white)
![.NET](https://img.shields.io/badge/.NET%208-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![MongoDB](https://img.shields.io/badge/MongoDB-47A248?style=for-the-badge&logo=mongodb&logoColor=white)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-FF6600?style=for-the-badge&logo=rabbitmq&logoColor=white)
![SignalR](https://img.shields.io/badge/SignalR-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Polly](https://img.shields.io/badge/Polly-EF2D5E?style=for-the-badge&logo=nuget&logoColor=white)
![Azure](https://img.shields.io/badge/Azure-0078D4?style=for-the-badge&logo=microsoftazure&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![CI/CD](https://img.shields.io/badge/CI%2FCD-0078D4?style=for-the-badge&logo=azure-pipelines&logoColor=white)

Collaborative task manager applied to the SCRUM methodology.

# Tech stack

*   **Frontend:** Angular, CSS.
*   **Backend:** ASP.NET Core, C#.
*   **Data:** MongoDB.
*   **AI Integration:** Gemini API for intelligent task organization assistance.
*   **DevOps:** Azure DevOps, Docker.

# Architecture and user sections

## Arquitecture

Robust distributed microservices architecture focused on scalability.

*   **API Gateway:** Custom implementation for centralized routing and security.
*   **Inter-service communication:** Hybrid approach using synchronous HTTP (with Polly for retries/resilience) and asynchronous messaging via **RabbitMQ** to ensure eventual consistency.
*   **Security:** OAuth2 integration (Google) and centralized JWT handling with Refresh Tokens.
*   **Real-time:** Integration of **SignalR** for live updates on task boards.

## Distributed data schema
```mermaid
classDiagram
    class User {
        +string Id
        +string Username
        +string Email
        +string GroupName
        +string GroupRole
    }

    class Token {
        +string Id
        +string RefreshToken
        +DateTimeOffset ExpirationTime
        +string UserId
    }

    class Group {
        +string Id
        +string Name
        +string OwnerName
        +bool IsScrum
        +string SprintId
    }

    class GroupsRoles {
        +string Id
        +string GroupName
        +string UserName
        +string RoleName
    }

    class Sprint {
        +string Id
        +string GroupName
        +int SprintNumber
        +string Status
        +DateTimeOffset SprintExpiration
    }

    class SprintRetrospective {
        +string Id
        +string SprintId
        +string GroupName
        +int Rating
        +string Feedback
    }

    class Feedback {
        +string Id
        +string SprintId
        +string Username
        +bool IsSubmited
    }

    class UserTask {
        +string Id
        +string Title
        +string Status
        +string Username
        +string GroupName
        +string SprintId
        +int Priority
        +List~TaskItem~ TaskItems
    }

    class TaskItem {
        +string Id
        +string TaskId
        +string TaskTitle
        +bool IsCompleted
        +string AssignToUsername
    }

    class Message {
        +string Id
        +string Content
        +string Sender
        +string DateId
    }

    class MessagesDate {
        +string Id
        +string Date
        +string GroupName
    }
    
    User "1" .. "*" Token : owns
    User "1" .. "*" UserTask : creates
    User "1" .. "*" Message : sends
    User "1" .. "1" GroupsRoles : has role in
    
    Group "1" .. "*" User : contains members
    Group "1" .. "*" Sprint : organizes
    Group "1" .. "*" UserTask : contains
    
    Sprint "1" .. "*" UserTask : includes
    Sprint "1" .. "1" SprintRetrospective : has
    
    UserTask "1" *-- "*" TaskItem : contains
    
    MessagesDate "1" *-- "*" Message : groups
```
# Sections

## User-role-based Group Section
Depending on their role, each user has specific options available within the group section:

![User-role-based interfaces](readme-assets/interfaces.png)

## Add Task (Backlog) Section
Users can add Backlog tasks to be carried out during a Sprint.

## My Assigned Tasks
Users can view a list of pending tasks or tasks assigned to them in the current group.

## Group Chat
Users in the group can send messages to facilitate communication. Messages are organized by the day they were sent.

## User Information
Users can view their role within the group and their username.

## Retrospectives
A Retrospective is submitted regarding the completed Sprint.

![sections](readme-assets/sections.png)

## Sprint Backlog Preparation
The Sprint Backlogs to be executed during a Sprint are prepared here.

![Sprints backlogs](readme-assets/sprints_backlogs_interface.png)

## Active Sprint
When a Sprint starts, it is visually highlighted, and a countdown timer showing the remaining time is added.

![Sprint started](readme-assets/sprint_started.png)

## Gemini API Assistant
Users can use the AI Assistant to help them complete their assigned tasks.

![Gemini API](readme-assets/gemini_api.gif)

## Sprint Backlog Sorting
Backlogs can be sorted according to their priority.

![Backlog sorting](readme-assets/backlog_sorting.gif)

## Sprint Retrospective
Just once a Sprint finishes, Developers and the Scrum Master can evaluate how the Sprint was carried out.

![Retrospective](readme-assets/retrospective1.gif)

## Sprints History
A history of Sprints can be viewed, detailing completed and uncompleted tasks (Backlogs).

![Sprints history](readme-assets/sprints_history.gif)

## Download Sprints History as PDF
A button to download the Sprint history report is available.

![Sprints pdf](readme-assets/sprints_pdf.gif)


# 🐳 How to Run with Docker

Ensure you have **Docker** and **Docker Compose** installed on your machine.

1. Download the `docker-compose.yml` and `.env` files.
2. Open a terminal in the project root directory.
3. Run the following command: `docker-compose up -d`

## Environment Configuration

To test specific external features, you must update the values in the `.env` file before running the containers.

### Google Gemini API  
#### If you want to use the Google API features, you need to provide your own valid API Key in the environment variable `GEMINI_API_KEY`
### Email notifications por password resetting 
#### To test the "Forgot Password" functionality, you need to configure a sender email account. For Gmail, you must use an **App Password** (not your login password), which you can generate in *Google Account > Security > 2-Step Verification > App passwords*. Then, set your Google email in `EMAIL` and the generated password in `PASSWORD`.

## Deployment strategy

### Microservices (Development & Docker)
In local development and Docker environments, the system runs as a fully distributed microservices architecture. Each service (Identity, Tasks, Gateway, etc.) operates in its own container, communicating via RabbitMQ and HTTP.

### Monolithic adaptation (Azure production)
For the live production demo on Azure, to adhere to the resource quotas of the `Azure Free Tier (F1)` (CPU/RAM limits), the services were consolidated into a single deployment unit (Monolith).
*   **Pipeline Strategy:** The `azure-pipelines.yml` included in the **TaskManagerMicroservices** folder demonstrates the CI/CD configuration for the microservices approach. However, a streamlined pipeline was executed for the production build, reusing the same business logic and domain code but deploying it as a unified application.

<div align="center">
  <br/>
  <a href="https://task-manager-client-fje4hnhnape7e0a3.canadacentral-01.azurewebsites.net" target="_blank">
    <img src="https://img.shields.io/badge/View_Live_Demo-Visit%20App-0078D4?style=for-the-badge&logo=microsoftazure&logoColor=white" alt="View Live Demo">
  </a>
  <br/>
</div>
