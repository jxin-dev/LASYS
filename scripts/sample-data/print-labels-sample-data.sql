-- ============================================================================
-- LASYS Sample Data Script for Print Label Tables
-- Database: otpclasysdb (MySQL)
-- ============================================================================

USE otpclasysdb;

-- ============================================================================
-- Clean up existing sample data (optional - comment out if you want to keep existing data)
-- ============================================================================
DELETE FROM prdprnt_ub_labels_tcl WHERE lot_no LIKE 'LOT202401%' OR lot_no LIKE 'LOT202402%' OR lot_no LIKE 'LOT202403%';
DELETE FROM prdprnt_cb_labels_tcl WHERE lot_no LIKE 'LOT202401%' OR lot_no LIKE 'LOT202402%' OR lot_no LIKE 'LOT202403%';
DELETE FROM prdprnt_oub_labels_tcl WHERE lot_no LIKE 'LOT202401%' OR lot_no LIKE 'LOT202402%' OR lot_no LIKE 'LOT202403%';
DELETE FROM prdprnt_aub_labels_tcl WHERE lot_no LIKE 'LOT202401%' OR lot_no LIKE 'LOT202402%' OR lot_no LIKE 'LOT202403%';
DELETE FROM prdprnt_acb_labels_tcl WHERE lot_no LIKE 'LOT202401%' OR lot_no LIKE 'LOT202402%' OR lot_no LIKE 'LOT202403%';
DELETE FROM prdprnt_ocb_labels_tcl WHERE lot_no LIKE 'LOT202401%' OR lot_no LIKE 'LOT202402%' OR lot_no LIKE 'LOT202403%';
DELETE FROM prdprnt_case_labels_tcl WHERE lot_no LIKE 'LOT202401%' OR lot_no LIKE 'LOT202402%' OR lot_no LIKE 'LOT202403%';

-- ============================================================================
-- Sample data for prdprnt_ub_labels_tcl (Unit Box labels)
-- ============================================================================
INSERT INTO prdprnt_ub_labels_tcl 
(item_code, lot_no, sequence, printer_name, batch_count, batch_set, 
 label_status, analyzer_result, visual_result, verifier_result,
 created_user, created_section, ip_address,
 lastupdate_user, lastupdate_section, lastupdate_ip,
 paired_type, approved_user, approved_section, approved_ip,
 created_datetime)
VALUES
('100090', 'LOT202401A', 1, 'PRINTER_UB_01', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '0109004', 11, '192.168.1.100', '0109004', 11, '192.168.1.100', '', '0109004', 11, '192.168.1.100', NOW(), NOW()),
('100090', 'LOT202401A', 2, 'PRINTER_UB_01', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '0109004', 11, '192.168.1.100', '0109004', 11, '192.168.1.100', '', '0109004', 11, '192.168.1.100', NOW(), NOW()),
('100090', 'LOT202401A', 3, 'PRINTER_UB_01', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '0109004', 11, '192.168.1.100', '0109004', 11, '192.168.1.100', '', '0109004', 11, '192.168.1.100', NOW(), NOW()),
('100090', 'LOT202401A', 4, 'PRINTER_UB_01', 1, 1, 'Replacement', 'Pass', 'Pass', 'Pass', '0109004', 11, '192.168.1.100', '0109004', 11, '192.168.1.100', '', '0109004', 11, '192.168.1.100', NOW(), NOW()),
('100090', 'LOT202401A', 5, 'PRINTER_UB_01', 1, 1, 'Returned', 'Pass', 'Pass', 'Pass', '0109004', 11, '192.168.1.100', '0109004', 11, '192.168.1.100', '', '0109004', 11, '192.168.1.100', NOW(), NOW()),
('100097', 'LOT202401B', 1, 'PRINTER_UB_01', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '0807193', 64, '192.168.1.101', '0807193', 64, '192.168.1.101', '', '0807193', 64, '192.168.1.101', NOW(), NOW()),
('100097', 'LOT202401B', 2, 'PRINTER_UB_01', 1, 1, 'Failed During Printing', 'Fail', 'Fail', 'Fail', '0807193', 64, '192.168.1.101', '0807193', 64, '192.168.1.101', '', '0807193', 64, '192.168.1.101', NOW(), NOW()),
('100098', 'LOT202401C', 1, 'PRINTER_UB_01', 1, 1, 'First', 'Pass', 'Pass', 'Pass', '2105001', 64, '192.168.1.102', '2105001', 64, '192.168.1.102', '', '2105001', 64, '192.168.1.102', NOW(), NOW()),
('100098', 'LOT202401C', 2, 'PRINTER_UB_01', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '2105001', 64, '192.168.1.102', '2105001', 64, '192.168.1.102', '', '2105001', 64, '192.168.1.102', NOW(), NOW()),
('100098', 'LOT202401C', 3, 'PRINTER_UB_01', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '2105001', 64, '192.168.1.102', '2105001', 64, '192.168.1.102', '', '2105001', 64, '192.168.1.102', NOW(), NOW()),
('100098', 'LOT202401C', 4, 'PRINTER_UB_01', 1, 1, 'Last', 'Pass', 'Pass', 'Pass', '2105001', 64, '192.168.1.102', '2105001', 64, '192.168.1.102', '', '2105001', 64, '192.168.1.102', NOW(), NOW()),
('100103', 'LOT202402A', 1, 'PRINTER_UB_02', 1, 1, 'Additional', 'Pass', 'Pass', 'Pass', '1807001', 64, '192.168.1.103', '1807001', 64, '192.168.1.103', '', '1807001', 64, '192.168.1.103', NOW(), NOW()),
('100103', 'LOT202402A', 2, 'PRINTER_UB_02', 1, 1, 'Additional', 'Pass', 'Pass', 'Pass', '1807001', 64, '192.168.1.103', '1807001', 64, '192.168.1.103', '', '1807001', 64, '192.168.1.103', NOW(), NOW()),
('100204', 'LOT202402B', 1, 'PRINTER_UB_02', 1, 1, 'Replacement', 'Pass', 'Pass', 'Pass', '1204086', 11, '192.168.1.104', '1204086', 11, '192.168.1.104', '', '1204086', 11, '192.168.1.104', NOW(), NOW()),
('100211', 'LOT202402C', 1, 'PRINTER_UB_02', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '2105001', 64, '192.168.1.107', '2105001', 64, '192.168.1.107', '', '2105001', 64, '192.168.1.107', NOW(), NOW()),
('100215', 'LOT202403C', 1, 'PRINTER_UB_02', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '2105001', 64, '192.168.1.108', '2105001', 64, '192.168.1.108', '', '2105001', 64, '192.168.1.108', NOW(), NOW()),
('100220', 'LOT202403D', 1, 'PRINTER_UB_02', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '2105001', 64, '192.168.1.109', '2105001', 64, '192.168.1.109', '', '2105001', 64, '192.168.1.109', NOW(), NOW()),
('100276', 'LOT202403A', 1, 'PRINTER_UB_03', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '1807001', 64, '192.168.1.105', '1807001', 64, '192.168.1.105', '', '1807001', 64, '192.168.1.105', NOW(), NOW()),
('100277', 'LOT202403B', 1, 'PRINTER_UB_03', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '1807001', 64, '192.168.1.106', '1807001', 64, '192.168.1.106', '', '1807001', 64, '192.168.1.106', NOW(), NOW());

-- ============================================================================
-- Sample data for prdprnt_cb_labels_tcl (Carton Box labels)
-- ============================================================================
INSERT INTO prdprnt_cb_labels_tcl 
(item_code, lot_no, sequence, printer_name, batch_count, batch_set, 
 label_status, analyzer_result, visual_result, verifier_result,
 created_user, created_section, ip_address,
 lastupdate_user, lastupdate_section, lastupdate_ip,
 paired_type, approved_user, approved_section, approved_ip,
 created_datetime)
VALUES
('100090', 'LOT202401A', 1, 'PRINTER_CB_01', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '0109004', 11, '192.168.1.100', '0109004', 11, '192.168.1.100', 'Paired', '0109004', 11, '192.168.1.100', NOW(), NOW()),
('100090', 'LOT202401A', 2, 'PRINTER_CB_01', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '0109004', 11, '192.168.1.100', '0109004', 11, '192.168.1.100', 'Paired', '0109004', 11, '192.168.1.100', NOW(), NOW()),
('100090', 'LOT202401A', 3, 'PRINTER_CB_01', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '0109004', 11, '192.168.1.100', '0109004', 11, '192.168.1.100', 'Paired', '0109004', 11, '192.168.1.100', NOW(), NOW()),
('100097', 'LOT202401B', 1, 'PRINTER_CB_01', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '0807193', 64, '192.168.1.101', '0807193', 64, '192.168.1.101', '', '0807193', 64, '192.168.1.101', NOW(), NOW()),
('100097', 'LOT202401B', 2, 'PRINTER_CB_01', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '0807193', 64, '192.168.1.101', '0807193', 64, '192.168.1.101', '', '0807193', 64, '192.168.1.101', NOW(), NOW()),
('100098', 'LOT202401C', 1, 'PRINTER_CB_01', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '2105001', 64, '192.168.1.102', '2105001', 64, '192.168.1.102', '', '2105001', 64, '192.168.1.102', NOW(), NOW()),
('100098', 'LOT202401C', 2, 'PRINTER_CB_01', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '2105001', 64, '192.168.1.102', '2105001', 64, '192.168.1.102', '', '2105001', 64, '192.168.1.102', NOW(), NOW()),
('100103', 'LOT202402A', 1, 'PRINTER_CB_02', 1, 1, 'Additional', 'Pass', 'Pass', 'Pass', '1807001', 64, '192.168.1.103', '1807001', 64, '192.168.1.103', '', '1807001', 64, '192.168.1.103', NOW(), NOW()),
('100276', 'LOT202403A', 1, 'PRINTER_CB_02', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '1807001', 64, '192.168.1.105', '1807001', 64, '192.168.1.105', '', '1807001', 64, '192.168.1.105', NOW(), NOW()),
('100276', 'LOT202403A', 2, 'PRINTER_CB_02', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '1807001', 64, '192.168.1.105', '1807001', 64, '192.168.1.105', '', '1807001', 64, '192.168.1.105', NOW(), NOW()),
('100277', 'LOT202403B', 1, 'PRINTER_CB_02', 1, 1, 'Returned', 'Pass', 'Pass', 'Pass', '1807001', 64, '192.168.1.106', '1807001', 64, '192.168.1.106', '', '1807001', 64, '192.168.1.106', NOW(), NOW());

-- ============================================================================
-- Sample data for prdprnt_oub_labels_tcl (Outer Unit Box labels)
-- ============================================================================
INSERT INTO prdprnt_oub_labels_tcl 
(item_code, lot_no, sequence, printer_name, batch_count, batch_set, 
 label_status, analyzer_result, visual_result, verifier_result,
 created_user, created_section, ip_address,
 lastupdate_user, lastupdate_section, lastupdate_ip,
 paired_type, approved_user, approved_section, approved_ip,
 created_datetime)
VALUES
('100204', 'LOT202402B', 1, 'PRINTER_OUB_01', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '1204086', 11, '192.168.1.104', '1204086', 11, '192.168.1.104', '', '1204086', 11, '192.168.1.104', NOW(), NOW()),
('100204', 'LOT202402B', 2, 'PRINTER_OUB_01', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '1204086', 11, '192.168.1.104', '1204086', 11, '192.168.1.104', '', '1204086', 11, '192.168.1.104', NOW(), NOW()),
('100204', 'LOT202402B', 3, 'PRINTER_OUB_01', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '1204086', 11, '192.168.1.104', '1204086', 11, '192.168.1.104', '', '1204086', 11, '192.168.1.104', NOW(), NOW()),
('100211', 'LOT202402C', 1, 'PRINTER_OUB_01', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '2105001', 64, '192.168.1.107', '2105001', 64, '192.168.1.107', '', '2105001', 64, '192.168.1.107', NOW(), NOW()),
('100211', 'LOT202402C', 2, 'PRINTER_OUB_01', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '2105001', 64, '192.168.1.107', '2105001', 64, '192.168.1.107', '', '2105001', 64, '192.168.1.107', NOW(), NOW()),
('100215', 'LOT202403C', 1, 'PRINTER_OUB_01', 1, 1, 'Additional', 'Pass', 'Pass', 'Pass', '2105001', 64, '192.168.1.108', '2105001', 64, '192.168.1.108', '', '2105001', 64, '192.168.1.108', NOW(), NOW()),
('100220', 'LOT202403D', 1, 'PRINTER_OUB_02', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '2105001', 64, '192.168.1.109', '2105001', 64, '192.168.1.109', '', '2105001', 64, '192.168.1.109', NOW(), NOW());

-- ============================================================================
-- Sample data for prdprnt_aub_labels_tcl (Additional Unit Box labels)
-- ============================================================================
INSERT INTO prdprnt_aub_labels_tcl 
(item_code, lot_no, sequence, printer_name, batch_count, batch_set, 
 label_status, analyzer_result, visual_result, verifier_result,
 created_user, created_section, ip_address,
 lastupdate_user, lastupdate_section, lastupdate_ip,
 paired_type, approved_user, approved_section, approved_ip,
 created_datetime)
VALUES
('100090', 'LOT202401A', 1, 'PRINTER_AUB_01', 1, 1, 'Additional', 'Pass', 'Pass', 'Pass', '0109004', 11, '192.168.1.100', '0109004', 11, '192.168.1.100', '', '0109004', 11, '192.168.1.100', NOW(), NOW()),
('100090', 'LOT202401A', 2, 'PRINTER_AUB_01', 1, 1, 'Additional', 'Pass', 'Pass', 'Pass', '0109004', 11, '192.168.1.100', '0109004', 11, '192.168.1.100', '', '0109004', 11, '192.168.1.100', NOW(), NOW()),
('100097', 'LOT202401B', 1, 'PRINTER_AUB_01', 1, 1, 'Additional', 'Pass', 'Pass', 'Pass', '0807193', 64, '192.168.1.101', '0807193', 64, '192.168.1.101', '', '0807193', 64, '192.168.1.101', NOW(), NOW()),
('100098', 'LOT202401C', 1, 'PRINTER_AUB_01', 1, 1, 'Replacement', 'Pass', 'Pass', 'Pass', '2105001', 64, '192.168.1.102', '2105001', 64, '192.168.1.102', '', '2105001', 64, '192.168.1.102', NOW(), NOW()),
('100103', 'LOT202402A', 1, 'PRINTER_AUB_01', 1, 1, 'Additional', 'Pass', 'Pass', 'Pass', '1807001', 64, '192.168.1.103', '1807001', 64, '192.168.1.103', '', '1807001', 64, '192.168.1.103', NOW(), NOW());

-- ============================================================================
-- Sample data for prdprnt_acb_labels_tcl (Additional Carton Box labels)
-- ============================================================================
INSERT INTO prdprnt_acb_labels_tcl 
(item_code, lot_no, sequence, printer_name, batch_count, batch_set, 
 label_status, analyzer_result, visual_result, verifier_result,
 created_user, created_section, ip_address,
 lastupdate_user, lastupdate_section, lastupdate_ip,
 paired_type, approved_user, approved_section, approved_ip,
 created_datetime)
VALUES
('100090', 'LOT202401A', 1, 'PRINTER_ACB_01', 1, 1, 'Additional', 'Pass', 'Pass', 'Pass', '0109004', 11, '192.168.1.100', '0109004', 11, '192.168.1.100', 'Paired', '0109004', 11, '192.168.1.100', NOW(), NOW()),
('100097', 'LOT202401B', 1, 'PRINTER_ACB_01', 1, 1, 'Additional', 'Pass', 'Pass', 'Pass', '0807193', 64, '192.168.1.101', '0807193', 64, '192.168.1.101', '', '0807193', 64, '192.168.1.101', NOW(), NOW()),
('100097', 'LOT202401B', 2, 'PRINTER_ACB_01', 1, 1, 'Additional', 'Pass', 'Pass', 'Pass', '0807193', 64, '192.168.1.101', '0807193', 64, '192.168.1.101', '', '0807193', 64, '192.168.1.101', NOW(), NOW()),
('100103', 'LOT202402A', 1, 'PRINTER_ACB_01', 1, 1, 'Replacement', 'Pass', 'Pass', 'Pass', '1807001', 64, '192.168.1.103', '1807001', 64, '192.168.1.103', '', '1807001', 64, '192.168.1.103', NOW(), NOW());

-- ============================================================================
-- Sample data for prdprnt_ocb_labels_tcl (Outer Carton Box labels)
-- ============================================================================
INSERT INTO prdprnt_ocb_labels_tcl 
(item_code, lot_no, sequence, printer_name, batch_count, batch_set, 
 label_status, analyzer_result, visual_result, verifier_result,
 created_user, created_section, ip_address,
 lastupdate_user, lastupdate_section, lastupdate_ip,
 paired_type, approved_user, approved_section, approved_ip,
 created_datetime)
VALUES
('100204', 'LOT202402B', 1, 'PRINTER_OCB_01', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '1204086', 11, '192.168.1.104', '1204086', 11, '192.168.1.104', '', '1204086', 11, '192.168.1.104', NOW(), NOW()),
('100204', 'LOT202402B', 2, 'PRINTER_OCB_01', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '1204086', 11, '192.168.1.104', '1204086', 11, '192.168.1.104', '', '1204086', 11, '192.168.1.104', NOW(), NOW()),
('100211', 'LOT202402C', 1, 'PRINTER_OCB_01', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '2105001', 64, '192.168.1.107', '2105001', 64, '192.168.1.107', '', '2105001', 64, '192.168.1.107', NOW(), NOW()),
('100215', 'LOT202403C', 1, 'PRINTER_OCB_01', 1, 1, 'Additional', 'Pass', 'Pass', 'Pass', '2105001', 64, '192.168.1.108', '2105001', 64, '192.168.1.108', '', '2105001', 64, '192.168.1.108', NOW(), NOW()),
('100220', 'LOT202403D', 1, 'PRINTER_OCB_02', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '2105001', 64, '192.168.1.109', '2105001', 64, '192.168.1.109', '', '2105001', 64, '192.168.1.109', NOW(), NOW());

-- ============================================================================
-- Sample data for prdprnt_case_labels_tcl (Case labels)
-- ============================================================================
INSERT INTO prdprnt_case_labels_tcl 
(item_code, lot_no, sequence, printer_name, batch_count, batch_set, 
 label_status, analyzer_result, visual_result, verifier_result,
 created_user, created_section, ip_address,
 lastupdate_user, lastupdate_section, lastupdate_ip,
 paired_type, approved_user, approved_section, approved_ip,
 created_datetime)
VALUES
('100090', 'LOT202401A', 1, 'PRINTER_CASE_01', 1, 1, 'First', 'Pass', 'Pass', 'Pass', '0109004', 11, '192.168.1.100', '0109004', 11, '192.168.1.100', '', '0109004', 11, '192.168.1.100', NOW(), NOW()),
('100090', 'LOT202401A', 2, 'PRINTER_CASE_01', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '0109004', 11, '192.168.1.100', '0109004', 11, '192.168.1.100', '', '0109004', 11, '192.168.1.100', NOW(), NOW()),
('100090', 'LOT202401A', 3, 'PRINTER_CASE_01', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '0109004', 11, '192.168.1.100', '0109004', 11, '192.168.1.100', '', '0109004', 11, '192.168.1.100', NOW(), NOW()),
('100090', 'LOT202401A', 4, 'PRINTER_CASE_01', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '0109004', 11, '192.168.1.100', '0109004', 11, '192.168.1.100', '', '0109004', 11, '192.168.1.100', NOW(), NOW()),
('100090', 'LOT202401A', 5, 'PRINTER_CASE_01', 1, 1, 'Last', 'Pass', 'Pass', 'Pass', '0109004', 11, '192.168.1.100', '0109004', 11, '192.168.1.100', '', '0109004', 11, '192.168.1.100', NOW(), NOW()),
('100097', 'LOT202401B', 1, 'PRINTER_CASE_01', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '0807193', 64, '192.168.1.101', '0807193', 64, '192.168.1.101', '', '0807193', 64, '192.168.1.101', NOW(), NOW()),
('100097', 'LOT202401B', 2, 'PRINTER_CASE_01', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '0807193', 64, '192.168.1.101', '0807193', 64, '192.168.1.101', '', '0807193', 64, '192.168.1.101', NOW(), NOW()),
('100098', 'LOT202401C', 1, 'PRINTER_CASE_01', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '2105001', 64, '192.168.1.102', '2105001', 64, '192.168.1.102', '', '2105001', 64, '192.168.1.102', NOW(), NOW()),
('100098', 'LOT202401C', 2, 'PRINTER_CASE_01', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '2105001', 64, '192.168.1.102', '2105001', 64, '192.168.1.102', '', '2105001', 64, '192.168.1.102', NOW(), NOW()),
('100103', 'LOT202402A', 1, 'PRINTER_CASE_02', 1, 1, 'Additional', 'Pass', 'Pass', 'Pass', '1807001', 64, '192.168.1.103', '1807001', 64, '192.168.1.103', '', '1807001', 64, '192.168.1.103', NOW(), NOW()),
('100276', 'LOT202403A', 1, 'PRINTER_CASE_02', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '1807001', 64, '192.168.1.105', '1807001', 64, '192.168.1.105', '', '1807001', 64, '192.168.1.105', NOW(), NOW()),
('100277', 'LOT202403B', 1, 'PRINTER_CASE_02', 1, 1, 'Original', 'Pass', 'Pass', 'Pass', '1807001', 64, '192.168.1.106', '1807001', 64, '192.168.1.106', '', '1807001', 64, '192.168.1.106', NOW(), NOW());

-- ============================================================================
-- Verification queries
-- ============================================================================
SELECT 'UB Labels' as TableName, COUNT(*) as RecordCount FROM prdprnt_ub_labels_tcl WHERE lot_no LIKE 'LOT2024%'
UNION ALL
SELECT 'CB Labels', COUNT(*) FROM prdprnt_cb_labels_tcl WHERE lot_no LIKE 'LOT2024%'
UNION ALL
SELECT 'OUB Labels', COUNT(*) FROM prdprnt_oub_labels_tcl WHERE lot_no LIKE 'LOT2024%'
UNION ALL
SELECT 'AUB Labels', COUNT(*) FROM prdprnt_aub_labels_tcl WHERE lot_no LIKE 'LOT2024%'
UNION ALL
SELECT 'ACB Labels', COUNT(*) FROM prdprnt_acb_labels_tcl WHERE lot_no LIKE 'LOT2024%'
UNION ALL
SELECT 'OCB Labels', COUNT(*) FROM prdprnt_ocb_labels_tcl WHERE lot_no LIKE 'LOT2024%'
UNION ALL
SELECT 'Case Labels', COUNT(*) FROM prdprnt_case_labels_tcl WHERE lot_no LIKE 'LOT2024%';

-- Show sample records from each table
SELECT 'UB Labels Sample' as Info, item_code, lot_no, sequence, label_status 
FROM prdprnt_ub_labels_tcl WHERE lot_no LIKE 'LOT2024%' LIMIT 5;

SELECT 'CB Labels Sample' as Info, item_code, lot_no, sequence, label_status 
FROM prdprnt_cb_labels_tcl WHERE lot_no LIKE 'LOT2024%' LIMIT 5;
