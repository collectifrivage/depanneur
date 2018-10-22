# Build frontend assets
FROM node:10 AS assets-build

WORKDIR /src

COPY package.json .
COPY package-lock.json .
RUN npm install

COPY webpack.config.js .
COPY Depanneur.App/Frontend/ ./Depanneur.App/Frontend/
RUN npm run webpack-prod

# Build web app
FROM microsoft/dotnet:2.1-sdk AS code-build

WORKDIR /src

COPY Depanneur.sln .
COPY Depanneur.App/Depanneur.App.csproj ./Depanneur.App/
RUN dotnet restore

COPY Depanneur.App/ ./Depanneur.App/
RUN dotnet publish Depanneur.App/ -o /publish

# Prepare runtime image
FROM microsoft/dotnet:2.1-aspnetcore-runtime AS runtime

WORKDIR /app

COPY --from=code-build /publish .
COPY --from=assets-build /src/Depanneur.App/wwwroot/bundles/ ./wwwroot/bundles/

ENTRYPOINT [ "dotnet", "Depanneur.App.dll" ]