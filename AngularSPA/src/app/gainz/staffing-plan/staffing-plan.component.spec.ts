import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StaffingPlanComponent } from './staffing-plan.component';

describe('StaffingPlanComponent', () => {
  let component: StaffingPlanComponent;
  let fixture: ComponentFixture<StaffingPlanComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StaffingPlanComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(StaffingPlanComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
