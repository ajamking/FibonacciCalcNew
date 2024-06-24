FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env

WORKDIR /App

ARG MICROSERVICE_NAME
COPY ./src .

RUN dotnet restore
RUN dotnet publish ${MICROSERVICE_NAME} -c Release -o bin

FROM mcr.microsoft.com/dotnet/aspnet:6.0

ARG MICROSERVICE_NAME

WORKDIR /App
COPY --from=build-env /App/bin .

ENV PROCESS=$MICROSERVICE_NAME

EXPOSE 8081

CMD dotnet "${PROCESS}.dll"