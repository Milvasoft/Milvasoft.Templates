
----------------------------------------------------------------------------------------------------------------
-- Auto increment start 21 (only the seed/system tables; all other tables keep their normal 1-based ids)

DO $$
DECLARE
    seq_record RECORD;
    schema_name TEXT;
    sequence_name TEXT;
BEGIN
    FOR seq_record IN
        SELECT
            table_name,
            column_name,
            pg_get_serial_sequence('"' || table_name || '"', column_name) AS seq_name
        FROM
            information_schema.columns
        WHERE
            table_schema = 'public'
            AND table_name IN ('Users', 'Roles', 'Permissions', 'Languages')
            AND pg_get_serial_sequence('"' || table_name || '"', column_name) IS NOT NULL
    LOOP
        sequence_name := split_part(seq_record.seq_name, '.', 2);

        EXECUTE format('ALTER SEQUENCE %s RESTART WITH 21;', sequence_name);
    END LOOP;
END $$;


----------------------------------------------------------------------------------------------------------------
-- Static Id column name in trigger function
----------------------------------------------------------------------------------------------------------------

----------------------------------------------------------------------------------------------------------------
-- Seed modification prevention trigger
CREATE OR REPLACE FUNCTION prevent_seed_modification()
RETURNS TRIGGER AS $$
BEGIN

    IF TG_OP = 'INSERT' THEN
        IF NEW."Id" < 21 THEN
            RAISE EXCEPTION 'Seed records cannot be modified.';
        END IF;
    ELSIF TG_OP = 'UPDATE' THEN
        IF NEW."Id" < 21 THEN
            RAISE EXCEPTION 'Seed records cannot be modified.';
        END IF;
    ELSIF TG_OP = 'DELETE' THEN
        IF OLD."Id" < 21 THEN
            RAISE EXCEPTION 'Seed records cannot be deleted.';
        END IF;
        RETURN OLD;
    END IF;

    RETURN NEW;

END;
$$ LANGUAGE plpgsql;


----------------------------------------------------------------------------------------------------------------
-- Add trigger to all tables


DO $$
DECLARE
    r RECORD;
BEGIN
    FOR r IN (
        SELECT
            c.relname AS table_name
        FROM
            pg_class c
        JOIN
            pg_namespace n ON n.oid = c.relnamespace
        JOIN
            pg_attribute a ON a.attrelid = c.oid
        JOIN
            pg_constraint con ON con.conrelid = c.oid AND con.contype = 'p' -- Primary Key
        WHERE
            n.nspname = 'public'
            AND a.attnum = ANY(con.conkey) -- Primary key kolonlarını filtrele
            AND a.attidentity IN ('a', 'd') -- Auto-increment identity kolonları
            AND c.relname IN ('Users', 'Roles', 'Permissions', 'Languages')
    ) LOOP
        -- Trigger ekle
        EXECUTE format('
            CREATE TRIGGER prevent_seed_modification_trigger
            BEFORE INSERT OR UPDATE OR DELETE ON %I
            FOR EACH ROW EXECUTE FUNCTION prevent_seed_modification();', r.table_name);
    END LOOP;
END $$;
