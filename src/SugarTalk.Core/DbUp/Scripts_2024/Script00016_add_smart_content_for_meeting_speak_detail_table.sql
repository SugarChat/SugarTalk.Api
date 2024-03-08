ALTER TABLE meeting_speak_detail
    ADD smart_content TEXT NULL;

ALTER TABLE meeting_speak_detail
    RENAME speak_content To original_content TEXT;