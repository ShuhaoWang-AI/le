PRINT 'START of ALTER TABLE SCRIPT'

-- change email template for lock manual lock

 UPDATE [DBO].[tAuditLogTemplate]
 SET MessageTemplate = '<html>
            <body> 
                <pre>
Hello,

For security reasons, your account has been locked by the Authority. Please contact your Authority for assistance unlocking your account.

    {authorityName} at {authoritySupportEmail} or {authoritySupportPhoneNumber}  

This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
                 </pre>
            </body>
        </html>'

 WHERE NAME = 'Email_UserAccess_AccountLockout'  


PRINT 'END of ALTER TABLE SCRIPT';