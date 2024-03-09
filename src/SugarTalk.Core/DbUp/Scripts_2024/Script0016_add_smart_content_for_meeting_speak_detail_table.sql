ALTER TABLE meeting_speak_detail
    ADD smart_content TEXT NULL;

ALTER TABLE meeting_speak_detail
    CHANGE COLUMN speak_content original_content TEXT;