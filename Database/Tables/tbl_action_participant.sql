CREATE TABLE [dbo].[tbl_action_participant] (
    [action_id]  INT           NOT NULL,
    [account_id] INT           NOT NULL,
    [contact_id] INT           NOT NULL,
    [action_dtm] SMALLDATETIME NOT NULL,
    [company_id] INT           NOT NULL,
    [recent_ind] BIT           NOT NULL
);


GO
CREATE CLUSTERED COLUMNSTORE INDEX [staging_tbl_action_participant_20180227-165228_ClusteredColumnStoreIndex-20180226-154919]
    ON [dbo].[tbl_action_participant];

