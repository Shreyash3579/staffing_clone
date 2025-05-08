import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EarlyInputFormComponent } from './early-input-form.component';

describe('EarlyInputFormComponent', () => {
  let component: EarlyInputFormComponent;
  let fixture: ComponentFixture<EarlyInputFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ EarlyInputFormComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(EarlyInputFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});