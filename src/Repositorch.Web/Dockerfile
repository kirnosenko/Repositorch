FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
RUN curl -sL https://deb.nodesource.com/setup_14.x | bash -
RUN apt-get install -y nodejs
WORKDIR /src
COPY ["Repositorch/Repositorch.csproj", "Repositorch/"]
COPY ["Repositorch.Data.Entities.Persistent.SqlServer/Repositorch.Data.Entities.Persistent.SqlServer.csproj", "Repositorch.Data.Entities.Persistent.SqlServer/"]
COPY ["Repositorch.Data.Entities.Persistent.PostgreSql/Repositorch.Data.Entities.Persistent.PostgreSql.csproj", "Repositorch.Data.Entities.Persistent.PostgreSql/"]
COPY ["Repositorch.Web/Repositorch.Web.csproj", "Repositorch.Web/"]
RUN dotnet restore "Repositorch.Web/Repositorch.Web.csproj"
COPY . .
WORKDIR "/src/Repositorch.Web"
RUN dotnet publish "Repositorch.Web.csproj" -c Release -o /app

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine AS deploy
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