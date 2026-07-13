FROM mcr.microsoft.com/dotnet/sdk:9.0
WORKDIR /app
COPY . .
RUN dotnet restore
EXPOSE 5038
ENV ASPNETCORE_URLS=http://+:5038
CMD ["dotnet", "run", "--project", "src/OpenPayrun.Api", "--no-launch-profile"]
