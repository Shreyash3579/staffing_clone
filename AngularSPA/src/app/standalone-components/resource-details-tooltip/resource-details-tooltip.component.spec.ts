import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ResourceDetailsTooltipComponent } from './resource-details-tooltip.component';

describe('ResourceDetailsTooltipComponent', () => {
  let component: ResourceDetailsTooltipComponent;
  let fixture: ComponentFixture<ResourceDetailsTooltipComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ResourceDetailsTooltipComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ResourceDetailsTooltipComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
