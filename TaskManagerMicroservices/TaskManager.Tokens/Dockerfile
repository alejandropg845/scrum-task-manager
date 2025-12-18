

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src


COPY ["TaskManager.Tokens/TaskManager.Tokens.csproj", "TaskManager.Tokens/"]
COPY ["TaskManager.Common/TaskManager.Common.csproj", "TaskManager.Common/"]


RUN dotnet restore "TaskManager.Tokens/TaskManager.Tokens.csproj"


COPY . .
WORKDIR "/src/TaskManager.Tokens"


RUN dotnet build "TaskManager.Tokens.csproj" -c Release -o /app/build
FROM build AS publish
RUN dotnet publish "TaskManager.Tokens.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskManager.Tokens.dll"]
