
create FUNCTION  [dbo].[fn_ActionSearch] (
	@CompanyId INT,
	@DateMin SMALLDATETIME = '1900-01-01',
	@DateMax SMALLDATETIME = '2079-06-06')
RETURNS TABLE  
AS  
RETURN 
	SELECT action_id, action_dtm
		FROM dbo.[tbl_action_participant] act
		WHERE 
			act.company_id = @CompanyId
			AND act.[action_dtm] BETWEEN @DateMin AND @DateMax
		group by action_id, action_dtm