@dotnet build App\Saber.csproj --configuration release -v q
@cmd /c gulp publish
@docker-compose ls >nul 2>nul
@if %errorlevel%==0 goto start
@echo Waiting for Docker Engine to start . . .
:waitloop
@timeout /t 10
@docker-compose ls >nul 2>nul
@if %errorlevel%==1 goto waitloop
:start
@cd App/bin/Release/Saber
@cmd /c docker compose up -d
@cd ../../../..
@npx cypress run --browser chrome --spec "cypress/e2e/all.cy.js"