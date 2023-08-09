# Documentetion:
This project uses JWT Authorization and Role-Based Authentication
For database operations I used PostgreSQL. I tried the container approach using Docker.
## Setup:
Login to the docker using the terminal (Optional):
```sh
docker login
```
### Run the command below:
```sh
docker run --name dozodashboard -e POSTGRES_PASSWORD=DozoDashBoard1234! -e POSTGRES_DB=DozoDashBoard -p 5432:5432 -d postgres:latest
```
A container with a PostgreSQL image will be generated.
* Once the container is running. You can access the database.
* The username will be "postgres" and the password will be "DozoDashBoard1234!" unless you change it and the container will run in localhost:5432 unless you change.
  
### Note: Configurations can be changed but note that connection strings in both projects should change too.
* (You can change the Connection Strings in "appsettings.json" in the .NET Project)
* After setting connection strings accordingly, Run the following commands in Package Manager Console:
```sh
Add-Migration InitialCreate
```
You can type whatever you want instead of "InitialCreate".
```sh
Update-Database
```
* After migrations, The tables should generate automatically in the database. 
### If all steps are executed without an issue. You can run the project.

## Get Started:
* Create a user with Register Method. 
* Login with the credentials you registered with.
* After a successful Login, a token (JWT) will be generated in response.
* With this token you can authorize users in the application.
