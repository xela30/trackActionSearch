
create FUNCTION  [dbo].[fn_ActionSearchByAccountsColumnStore] (
	@AccountsIds dbo.[IntType] READONLY,
	@CompanyId INT,
	@DateMin SMALLDATETIME = '1900-01-01',
	@DateMax SMALLDATETIME = '2079-06-06')
RETURNS TABLE  
AS  
RETURN 
	SELECT action_id, action_dtm
		FROM dbo.[tbl_action_participant] act
		INNER JOIN @AccountsIds a on a.ID = act.account_id
		WHERE 
			act.company_id = @CompanyId
			AND act.[action_dtm] BETWEEN @DateMin AND @DateMax
		group by action_id, action_dtm