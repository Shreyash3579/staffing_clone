import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { AddTeamSkuComponent } from './add-team-sku.component';

describe('AddTeamSkuComponent', () => {
  let component: AddTeamSkuComponent;
  let fixture: ComponentFixture<AddTeamSkuComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ AddTeamSkuComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AddTeamSkuComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
