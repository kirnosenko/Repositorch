FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
RUN curl -sL https://deb.nodesource.com/setup_14.x | bash -
RUN apt-get install -y nodejs
WORKDIR "/build"
COPY ["src/Repositorch/Repositorch.csproj", "src/Repositorch/"]
COPY ["src/Repositorch.Data.Entities.Persistent/Repositorch.Data.Entities.Persistent.csproj", "src/Repositorch.Data.Entities.Persistent/"]
COPY ["src/Repositorch.Data.Entities.Persistent.SqlServer/Repositorch.Data.Entities.Persistent.SqlServer.csproj", "src/Repositorch.Data.Entities.Persistent.SqlServer/"]
COPY ["src/Repositorch.Data.Entities.Persistent.PostgreSql/Repositorch.Data.Entities.Persistent.PostgreSql.csproj", "src/Repositorch.Data.Entities.Persistent.PostgreSql/"]
COPY ["src/Repositorch.Web/Repositorch.Web.csproj", "src/Repositorch.Web/"]
RUN dotnet restore "src/Repositorch.Web/Repositorch.Web.csproj"
COPY . .
WORKDIR "/build/src/Repositorch.Web"
RUN dotnet publish "Repositorch.Web.csproj" -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS deploy
RUN apk add --no-cache git \
	icu-libs
RUN git config --global core.quotepath off && \
	git config --global diff.renameLimit 999999 && \
	git config --global merge.renameLimit 999999
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
WORKDIR /app
COPY --from=build /app .
EXPOSE 80
ENTRYPOINT ["dotnet", "Repositorch.Web.dll"]