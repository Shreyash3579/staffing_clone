import { Injectable } from '@angular/core';
import { CoreService } from './core/core.service';

@Injectable()
export class AppInitializerService {

  constructor(private coreService: CoreService) { }

//   initializeApp(): Promise<void> {
//     return new Promise((resolve, reject) => {
//       const configDeps: Promise<void>[] = [];

//       this.coreService.loadEmployee().toPromise()
//       .then(() => {
//         if (this.coreService.loggedInUser.token) {
//           configDeps.push(
//             this.coreService.loadAllUserPreferences().toPromise()
//           );
//         }
//         return Promise.all(configDeps);
//       })
//       .then(() => {
//         resolve();
//       });
//   });
// }
}
