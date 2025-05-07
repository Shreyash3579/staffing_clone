import { TestBed } from '@angular/core/testing';

import { GainzService } from './gainz.service';

describe('GainzService', () => {
  let service: GainzService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(GainzService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
