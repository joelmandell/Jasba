# 04-WebApp-Layer: WebApp Validation

## Objective
Validate that all WebApp functionality works correctly for all three roles.

## Validation Checklist

### SystemAdmin Tests
- [ ] Can login as admin@sbapro.com
- [ ] Can view /Admin/Tenants page
- [ ] Can create new tenant
- [ ] Can create tenant admin user
- [ ] Cannot access tenant-specific pages

### TenantAdmin Tests
- [ ] Can login as demo@democompany.se
- [ ] Can view /Tenant/Sites page
- [ ] Can create/edit/delete sites
- [ ] Can upload floor plans
- [ ] Can place objects on floor plans
- [ ] Can create object types
- [ ] Can create inspector users
- [ ] Cannot see other tenant's data

### Inspector Tests
- [ ] Can login as inspector@democompany.se
- [ ] Can view /Inspector/Rounds page
- [ ] Can start new inspection round
- [ ] Can mark objects as OK/Issue
- [ ] Can add comments
- [ ] Can complete round
- [ ] Can download PDF report

### Security Tests
- [ ] Unauthenticated users redirected to login
- [ ] Role-based authorization enforced
- [ ] Tenant isolation maintained
- [ ] Cannot manipulate URLs to access other tenant data

### UI/UX Tests
- [ ] All pages render correctly
- [ ] Forms validate input
- [ ] Error messages display
- [ ] Success messages display
- [ ] Navigation works
- [ ] Responsive design works

## Success Criteria

✅ All role-specific features work  
✅ Authentication and authorization enforced  
✅ Tenant isolation maintained  
✅ UI is user-friendly  
✅ No console errors  
✅ End-to-end workflows complete  

## Next Steps

After successful validation:
1. Proceed to Phase 5: Advanced Features
2. Document any issues found
3. Refine UI/UX as needed
