import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SidebarProjectListComponent } from './sidebar-project-list.component';

describe('SidebarProjectListComponent', () => {
  let component: SidebarProjectListComponent;
  let fixture: ComponentFixture<SidebarProjectListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SidebarProjectListComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(SidebarProjectListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
