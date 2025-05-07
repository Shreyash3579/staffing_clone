import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LeadershipComponentComponent } from '../leadership-component.component';


describe('LeadershipComponentComponent', () => {
  let component: LeadershipComponentComponent;
  let fixture: ComponentFixture<LeadershipComponentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ LeadershipComponentComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(LeadershipComponentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});