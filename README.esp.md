<div align="center">
  <a href="./README.md">
    <img src="https://img.shields.io/badge/Read_in_English-EN-blue?style=for-the-badge" alt="Read in English">
  </a>
</div>

# Gestor de tareas colaborativo con enfoque en la metodología SCRUM

![Angular](https://img.shields.io/badge/Angular-DD0031?style=for-the-badge&logo=angular&logoColor=white)
![.NET](https://img.shields.io/badge/.NET%208-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![MongoDB](https://img.shields.io/badge/MongoDB-47A248?style=for-the-badge&logo=mongodb&logoColor=white)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-FF6600?style=for-the-badge&logo=rabbitmq&logoColor=white)
![SignalR](https://img.shields.io/badge/SignalR-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Polly](https://img.shields.io/badge/Polly-EF2D5E?style=for-the-badge&logo=nuget&logoColor=white)
![Azure](https://img.shields.io/badge/Azure-0078D4?style=for-the-badge&logo=microsoftazure&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![CI/CD](https://img.shields.io/badge/CI%2FCD-0078D4?style=for-the-badge&logo=azure-pipelines&logoColor=white)
![xUnit](https://img.shields.io/badge/xUnit-Framework_de_pruebas-448811?style=for-the-badge&logo=xunit&logoColor=white)
![FluentAssertions](https://img.shields.io/badge/FluentAssertions-Aserciones-0050C5?style=for-the-badge&logo=fluentassertions&logoColor=white)

Gestor de tareas colaborativo aplicado a la metodología SCRUM.

# Tecnologías

*   **Frontend:** Angular, CSS.
*   **Backend:** ASP.NET Core, C#.
*   **Datos:** MongoDB.
*   **Integración IA:** API de Gemini para asistencia inteligente en la organización de tareas.
*   **DevOps:** Azure DevOps, Docker.
*   **Testing:** Pruebas unitarias implementadas con xUnit y FluentAssertions para casos de prueba robustos y legibles.


# Arquitectura y secciones de usuario

## Arquitectura

Arquitectura de microservicios distribuida robusta enfocada en la escalabilidad.

*   **API Gateway:** Implementación personalizada para enrutamiento y seguridad centralizada.
*   **Comunicación entre servicios:** Enfoque híbrido utilizando HTTP síncrono (con Polly para reintentos y resiliencia) y mensajería asíncrona vía RabbitMQ para asegurar consistencia eventual.
*   **Seguridad:** Integración OAuth2 (Google) y manejo centralizado de JWT con Refresh Tokens.
*   **Tiempo real:** Integración de SignalR para actualizaciones en vivo en los tableros de tareas.

## Esquema de datos distribuido
```mermaid
classDiagram
%% Identity Context
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


# Secciones

## Sección de Grupo basada en Roles
Dependiendo de su rol, cada usuario tiene opciones específicas disponibles dentro de la sección de grupo:

![Interfaces basadas en roles](readme-assets/interfaces.png)

## Sección Agregar Tarea (Backlog)
Los usuarios pueden agregar tareas al Backlog para ser realizadas durante un Sprint.

## Mis Tareas Asignadas
Los usuarios pueden ver una lista de tareas pendientes o asignadas a ellos en el grupo actual.

## Chat de Grupo
Los usuarios del grupo pueden enviar mensajes para facilitar la comunicación. Los mensajes se organizan por el día en que fueron enviados.

## Información de Usuario
Los usuarios pueden ver su rol dentro del grupo y su nombre de usuario.

## Retrospectivas
Se envía una Retrospectiva con respecto al Sprint completado.

![Secciones](readme-assets/sections.png)

## Preparación del Sprint Backlog
Aquí se preparan los Backlogs del Sprint que se ejecutarán durante un Sprint.

![Backlogs de Sprint](readme-assets/sprints_backlogs_interface.png)

## Sprint Activo
Cuando comienza un Sprint, se resalta visualmente y se agrega un temporizador de cuenta regresiva que muestra el tiempo restante.

![Sprint iniciado](readme-assets/sprint_started.png)

## Asistente Gemini API
Los usuarios pueden usar el Asistente de IA para ayudarles a completar sus tareas asignadas.

![Gemini API](readme-assets/gemini_api.gif)

## Ordenamiento del Sprint Backlog
Los Backlogs se pueden ordenar según su prioridad.

![Ordenamiento de Backlog](readme-assets/backlog_sorting.gif)

## Retrospectiva del Sprint
Justo cuando finaliza un Sprint, los Desarrolladores y el Scrum Master pueden evaluar cómo se llevó a cabo el Sprint.

![Retrospectiva](readme-assets/retrospective1.gif)

## Historial de Sprints
Se puede ver un historial de Sprints, detallando las tareas completadas y no completadas (Backlogs).

![Historial de Sprints](readme-assets/sprints_history.gif)

## Descargar Historial de Sprints como PDF
Está disponible un botón para descargar el informe del historial de Sprints.

![Sprints pdf](readme-assets/sprints_pdf.gif)


# 🐳 Cómo ejecutar con Docker

Asegúrate de tener **Docker** y **Docker Compose** instalados en tu máquina.

1. Descarga los archivos `docker-compose.yml` y `.env`.
2. Abre una terminal en el directorio raíz del proyecto.
3. Ejecuta el siguiente comando: `docker-compose up -d`

## Configuración de Entorno

Para probar características externas específicas, debes actualizar los valores en el archivo `.env` antes de ejecutar los contenedores.

### Google Gemini API  
#### Si deseas utilizar las funciones de la API de Google, necesitas proporcionar tu propia API Key válida en la variable de entorno `GEMINI_API_KEY`.

### Notificaciones por correo para restablecimiento de contraseña
#### Para probar la funcionalidad de "Olvidé mi contraseña", necesitas configurar una cuenta de correo remitente. Para Gmail, debes usar una **Contraseña de Aplicación** (no tu contraseña de inicio de sesión), la cual puedes generar en *Cuenta de Google > Seguridad > Verificación en 2 pasos > Contraseñas de aplicaciones*. Luego, agrega tu correo de Google en `EMAIL` y la contraseña generada en `PASSWORD`.

## Estrategia de despliegue

### Microservicios (Desarrollo y Docker)
En entornos de desarrollo local y Docker, el sistema se ejecuta como una arquitectura de microservicios totalmente distribuida. Cada servicio (Identidad, Tareas, Gateway, etc.) opera en su propio contenedor, comunicándose a través de RabbitMQ y HTTP.

### Adaptación monolítica (Producción en Azure)
Para el despliegue, y para cumplir con las cuotas del `Azure Free Tier (F1)` (límites de CPU/RAM), los servicios se consolidaron en una única unidad de despliegue (Monolito).
*   **Estrategia de Pipeline:** El archivo `azure-pipelines.yml` incluido en la carpeta **TaskManagerMicroservices** demuestra la configuración de CI/CD para el enfoque de microservicios. Sin embargo, se ejecutó un pipeline simplificado para la compilación de producción, reutilizando la misma lógica de negocio y código de dominio pero desplegándolo como una aplicación unificada.

<div align="center">
  <br/>
  <a href="https://task-manager-client-fje4hnhnape7e0a3.canadacentral-01.azurewebsites.net" target="_blank">
    <img src="https://img.shields.io/badge/Ver_Demo_en_Vivo-Visitar%20App-0078D4?style=for-the-badge&logo=microsoftazure&logoColor=white" alt="Ver Demo en Vivo">
  </a>
  <br/>
</div>
