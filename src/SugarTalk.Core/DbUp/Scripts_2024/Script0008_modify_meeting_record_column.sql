CREATE INDEX idx_meeting_id ON meeting_record (meeting_id);
CREATE INDEX idx_meeting_id ON meeting_user_session (meeting_id);
alter table meeting_record
    add record_number varchar(36) null;

