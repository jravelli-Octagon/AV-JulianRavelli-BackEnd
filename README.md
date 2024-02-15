# AV-JulianRavelli-BackEnd
 Challenge para America Virtual

Web API en netcore8.
	Tiene 2 servicios. Uno para el manejo de usuarios y otro para consumir el servicio de consulta de clima.

	PASOS PARA CORRER EL BACKEND:
		1. Cambiar el appsettings el sv de base de datos (MSSQL) que tengan (actualmente apunta a mi motor local asi que no va a andar). También las credenciales.
		2. En la Consola del Administrador de Paquetes el comando "Update-Database -Context AmericaVirtualContext" va a crear la base de datos necesaria (la migration ya esta creada).
		3. En la base de datos ejecutar el siguiente script (genera el registro para la validacion de las contraseñas de usuarios).
			INSERT INTO PasswordPolicy VALUES (8, 25, 60, 3, 1, 1, 1 )
		4. En el repositorio de backend hay una collection de postman con el reqest CreateUser. Reemplazar con los valores deseado respetando los parametros de la tabla PasswordPolicy para la contraseña.
			
	La aplicacion (https) corre en el puerto 44307 (el FE esta configurado para consumir datos por ese puerto).
	En https://localhost:44307/swagger/index.html se puede el swagger una descripcion de los endpoints. Hay dos enpoints dentro del UserController (recupero de usuario y reseteo de contraseña) que no son usados por el FE pero son relevantes a la hora de demostrar conocimientos. 
	Soy conciente que en el appsettings hay informacion expuesta que no debería estar ahi en un proyecto de verdad.
	La documentacion de la api utilizada para los datos es https://www.meteosource.com/es/documentation.