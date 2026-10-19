DELETE FROM "InternalNotifications"
WHERE "SeenDate" < now()  - interval '30 days'
	or ("SeenDate" IS NULL AND "CreationDate" < now() - interval '60 days');