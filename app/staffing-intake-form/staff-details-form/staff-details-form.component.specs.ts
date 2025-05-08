import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StaffDetailsFormComponent } from './staff-details-form.component';

describe('StaffDetailsFormComponent', () => {
  let component: StaffDetailsFormComponent;
  let fixture: ComponentFixture<StaffDetailsFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ StaffDetailsFormComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(StaffDetailsFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});