# Archivo: Scripts_BD\002_create_database_user.sql
# Script SQL para crear el usuario de base de datos usado por la aplicacion.

# CREACION DEL USUARIO DE BASE DE DATOS
CREATE USER 'cineroadmap'@'%' IDENTIFIED BY '1234';

# DAR PRIVILEGIOS A LA BASE DE DATOS DE LA APLICACION
GRANT ALL PRIVILEGES ON cineroadmap.* TO 'cineroadmap'@'%';

# ACTUALIZAR PRIVILEGIOS
FLUSH PRIVILEGES;