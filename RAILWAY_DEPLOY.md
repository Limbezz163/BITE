# Deploy Yammy to Railway

## 1. Push latest changes to GitHub
```powershell
git add .
git commit -m "Prepare project for Railway deploy"
git push
```

## 2. Create project on Railway
1. Open Railway and create a new project from GitHub repo.
2. Select repository `Limbezz163/BITE`.
3. Railway will detect `Dockerfile` and build the app.

## 3. Add MySQL plugin
1. In the same Railway project, add `MySQL` service.
2. Open your app service (not MySQL) and add variables:
   - `MYSQLHOST` (reference from MySQL)
   - `MYSQLPORT` (reference from MySQL)
   - `MYSQLDATABASE` (reference from MySQL)
   - `MYSQLUSER` (reference from MySQL)
   - `MYSQLPASSWORD` (reference from MySQL)

## 4. Initialize database schema
Run in Railway app service shell:
```bash
mysql -h "$MYSQLHOST" -P "$MYSQLPORT" -u "$MYSQLUSER" -p"$MYSQLPASSWORD" "$MYSQLDATABASE" < Yammy.Backend/railway-init.sql
```

## 5. Verify deployment
1. Open generated Railway domain.
2. Check health endpoint: `/api/health`.
3. Open `/catalog.html` and verify restaurants API calls succeed.

## Notes
- Frontend is served by ASP.NET backend from the same domain.
- API base URL in frontend is automatic: local (`localhost`) or production (`/api`).
