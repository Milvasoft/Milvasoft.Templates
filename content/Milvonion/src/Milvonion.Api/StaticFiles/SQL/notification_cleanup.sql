DELETE FROM "InternalNotifications"
WHERE "SeenDate" < now()  - interval '30 days'
	or ("SeenDate" IS NULL AND "CreatedDate" < now() - interval '60 days');