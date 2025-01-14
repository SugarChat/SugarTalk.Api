alter table `meeting_summary_pdf_record`
    add column `pdf_export_type` int not null;

alter table `meeting_summary_pdf_record`
    change column `summary_id` `record_id` varchar(255) not null;

ALTER TABLE `meeting_summary_pdf_record` RENAME TO `meeting_record_pdf_record`;