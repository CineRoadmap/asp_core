-- ARCHIVO: Scripts_BD\002_validation_and_counts.sql
-- Consultas de auditoría para verificar la correcta importación de la base de datos semilla, integridad estructural y conteos.

USE cineroadmap;

-- 1. CONTEOS TOTALES

SELECT 
    (SELECT COUNT(*) FROM usuarios) AS 'Total Usuarios',
    (SELECT COUNT(*) FROM insignias) AS 'Total Insignias',
    (SELECT COUNT(*) FROM logros) AS 'Total Logros',
    (SELECT COUNT(*) FROM catalogo_retos) AS 'Total Retos en Catálogo',
    (SELECT COUNT(*) FROM peliculas) AS 'Total Películas Base',
    (SELECT COUNT(*) FROM actores) AS 'Total Actores Base',
    (SELECT COUNT(*) FROM directores) AS 'Total Directores Base',
    (SELECT COUNT(*) FROM generos) AS 'Total Géneros Base';


-- 2. VERIFICACIÓN DE INTEGRIDAD REFERENCIAL (Logros enlazados a Insignias)

-- Muestra si existen logros que se hayan quedado huérfanos (debe devolver 0 filas)
SELECT COUNT(*) AS 'Logros Huérfanos (Error)' 
FROM logros 
WHERE idInsignia IS NULL;

-- Muestra un desglose de los logros con sus respectivas imágenes de insignia asociadas
SELECT 
    l.idLogro, 
    l.nombreReto, 
    l.tipo_requisito, 
    l.valor_requisito, 
    i.nombre AS 'Insignia Asociada', 
    i.srcImagen
FROM logros l
INNER JOIN insignias i ON l.idInsignia = i.idInsignia
LIMIT 5;


-- 3. VERIFICACIÓN DE CONFIGURACIÓN DE CATALOGO DE RETOS

SELECT 
    tipo AS 'Tipo de Reto', 
    COUNT(*) AS 'Cantidad', 
    AVG(progreso_objetivo) AS 'Progreso Objetivo Promedio'
FROM catalogo_retos
GROUP BY tipo;