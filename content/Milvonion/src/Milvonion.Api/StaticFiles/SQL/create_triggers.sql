
----------------------------------------------------------------------------------------------------------------
-- Auto increment start 21 

DO $$
DECLARE
    seq_record RECORD;
BEGIN
    FOR seq_record IN
        SELECT table_name, column_name, pg_get_serial_sequence('"' || table_name || '"', column_name) AS seq_name
		FROM information_schema.columns
		WHERE table_schema = 'public' AND data_type = 'integer' AND column_default LIKE 'nextval%'
    LOOP
        -- Sequence başlangıç değerini güncelle
        EXECUTE format('ALTER SEQUENCE %s RESTART WITH 21;', seq_record.seq_name);
    END LOOP;
END $$;

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
    END IF;

    RETURN NEW; -- RETURN OLD; if TG_OP = 'DELETE'
END;
$$ LANGUAGE plpgsql;

----------------------------------------------------------------------------------------------------------------
-- Add trigger to all tables

DO $$ 
DECLARE
    r RECORD;
BEGIN
    FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = 'public') LOOP
        EXECUTE format('
            CREATE TRIGGER prevent_seed_modification_trigger
            BEFORE INSERT OR UPDATE ON %I
            FOR EACH ROW EXECUTE FUNCTION prevent_seed_modification();', r.tablename);
    END LOOP;
END $$;


