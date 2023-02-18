@echo -------------------------------------------------------------------------------------------
@echo Building Saber release
@echo -------------------------------------------------------------------------------------------
@dotnet build App\Saber.csproj -v q --configuration Release
@if not %errorlevel%==0 echo Build failed!
@if not %errorlevel%==0 goto finish
@echo -------------------------------------------------------------------------------------------
@echo Publishing Saber release
@echo -------------------------------------------------------------------------------------------
@cmd /c gulp publish
@docker-compose ls >nul 2>nul
@if %errorlevel%==0 goto start
@echo Waiting for Docker Engine to start . . .
:waitloop
@timeout /t 10
@docker-compose ls >nul 2>nul
@if %errorlevel%==1 goto waitloop
:start
@echo -------------------------------------------------------------------------------------------
@echo Running Saber release in Docker (http://localhost:7070)
@echo -------------------------------------------------------------------------------------------
@RM "App\bin\Release\Saber\docker-compose.yml"
@COPY "Publish\docker-compose-tests.yml" "App\bin\Release\Saber\"
@cd "App\bin\Release\Saber"
@MV docker-compose-tests.yml docker-compose.yml
@cmd /c docker compose down --rmi "all"
@cmd /c docker compose up -d
@cd "..\..\..\..\"
@echo -------------------------------------------------------------------------------------------
@echo Running Cypress E2E tests
@echo -------------------------------------------------------------------------------------------
@cmd /c npx cypress run --browser chrome --spec "cypress/e2e/all.cy.js"

@echo -------------------------------------------------------------------------------------------
@echo Cleaning up (shutting down Docker container and removing Docker image)
@echo -------------------------------------------------------------------------------------------
@cd "App\bin\Release\Saber"
@cmd /c docker compose down --rmi "all"
@cd "..\..\..\..\"
@RM "App\bin\Release\Saber\docker-compose.yml"
@COPY "Publish\docker-compose.yml" "App\bin\Release\Saber\"
@echo Done!
:finish