# Plan de respaldo y restauración

## Objetivo

Evitar pérdida de datos durante pruebas, entregas o cambios de estructura en la base de datos `cineroadmap`.

## Respaldo manual

Con Docker levantado, crear un backup SQL:

```powershell
docker compose exec mysql mysqldump -u root -p cineroadmap > backups/cineroadmap-backup.sql
```

Si se usa MySQL instalado localmente:

```powershell
mysqldump -h localhost -P 3307 -u root -p cineroadmap > backups/cineroadmap-backup.sql
```

La carpeta `backups/` debe mantenerse fuera de Git si contiene datos reales.

## Restauración

Restaurar un backup en el contenedor:

```powershell
docker compose exec -T mysql mysql -u root -p cineroadmap < backups/cineroadmap-backup.sql
```

Después de restaurar, comprobar tablas principales:

```sql
SELECT COUNT(*) FROM usuarios;
SELECT COUNT(*) FROM peliculas;
SELECT COUNT(*) FROM valoraciones;
SELECT COUNT(*) FROM usuario_retos;
```

## Checklist

- El backup se crea sin errores.
- El archivo SQL no se sube al repositorio.
- La restauración se prueba al menos una vez.
- La aplicación arranca después de restaurar.
