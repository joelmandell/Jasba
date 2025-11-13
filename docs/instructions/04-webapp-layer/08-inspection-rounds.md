# 04-WebApp-Layer: Inspector Inspection Rounds

## Objective
Implement the Inspector interface for executing inspection rounds.

## Prerequisites
- Completed: 04-webapp-layer/07-object-placement.md

## Instructions

Inspector pages allow inspectors to:
- View available sites
- Start new inspection rounds
- Execute inspections (mark objects as OK/Issue)
- Add comments
- Complete rounds

**Key Files**:
- `/Inspector/Rounds` - List of inspection rounds
- `/Inspector/StartRound` - Start new round
- `/Inspector/ExecuteRound/{id}` - Execute inspection

See existing implementations in `src/SBAPro.WebApp/Components/Pages/Inspector/`

## Workflow

1. Inspector logs in
2. Views list of sites
3. Starts new inspection round for a site
4. System loads all inspection objects for that site
5. Inspector checks each object:
   - Mark as "OK" or "Issue"
   - Add optional comment
6. Inspector completes round
7. System sets CompletedAt timestamp
8. PDF report available for download

## Validation

1. Login as inspector@democompany.se
2. Navigate to /Inspector/StartRound
3. Select a site and start round
4. Navigate to /Inspector/ExecuteRound/{roundId}
5. Mark objects as OK/Issue
6. Add comments
7. Complete round
8. Verify round status changes to "Completed"

## Success Criteria

✅ Can start new rounds  
✅ Can view objects to inspect  
✅ Can mark as OK/Issue  
✅ Can add comments  
✅ Can complete rounds  
✅ Timestamps recorded correctly  
