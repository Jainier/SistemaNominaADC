SET NOCOUNT ON;

-- Convierte valores historicos guardados como Id de AspNetUsers a UserName.
-- Solo actualiza cuando encuentra coincidencia exacta con AspNetUsers.Id.

UPDATE p
SET p.IdentityUserIdDecision = u.UserName
FROM Permiso p
INNER JOIN AspNetUsers u ON u.Id = p.IdentityUserIdDecision
WHERE p.IdentityUserIdDecision IS NOT NULL
  AND u.UserName IS NOT NULL;

UPDATE h
SET h.IdentityUserIdDecision = u.UserName
FROM SolicitudHorasExtra h
INNER JOIN AspNetUsers u ON u.Id = h.IdentityUserIdDecision
WHERE h.IdentityUserIdDecision IS NOT NULL
  AND u.UserName IS NOT NULL;

UPDATE v
SET v.IdentityUserIdDecision = u.UserName
FROM SolicitudVacaciones v
INNER JOIN AspNetUsers u ON u.Id = v.IdentityUserIdDecision
WHERE v.IdentityUserIdDecision IS NOT NULL
  AND u.UserName IS NOT NULL;

UPDATE i
SET i.IdentityUserIdDecision = u.UserName
FROM Incapacidad i
INNER JOIN AspNetUsers u ON u.Id = i.IdentityUserIdDecision
WHERE i.IdentityUserIdDecision IS NOT NULL
  AND u.UserName IS NOT NULL;

UPDATE n
SET n.IdentityUserIdDecision = u.UserName
FROM PlanillaEncabezado n
INNER JOIN AspNetUsers u ON u.Id = n.IdentityUserIdDecision
WHERE n.IdentityUserIdDecision IS NOT NULL
  AND u.UserName IS NOT NULL;
