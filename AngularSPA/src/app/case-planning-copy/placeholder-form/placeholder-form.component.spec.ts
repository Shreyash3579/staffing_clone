import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { PlaceholderFormComponent } from './placeholder-form.component';

describe('PlaceholderFormComponent', () => {
  let component: PlaceholderFormComponent;
  let fixture: ComponentFixture<PlaceholderFormComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ PlaceholderFormComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PlaceholderFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
