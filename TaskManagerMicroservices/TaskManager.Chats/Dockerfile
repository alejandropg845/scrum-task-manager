

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src


COPY ["TaskManager.Chats/TaskManager.Chats.csproj", "TaskManager.Chats/"]
COPY ["TaskManager.Common/TaskManager.Common.csproj", "TaskManager.Common/"]


RUN dotnet restore "TaskManager.Chats/TaskManager.Chats.csproj"


COPY . .
WORKDIR "/src/TaskManager.Chats"


RUN dotnet build "TaskManager.Chats.csproj" -c Release -o /app/build
FROM build AS publish
RUN dotnet publish "TaskManager.Chats.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskManager.Chats.dll"]
