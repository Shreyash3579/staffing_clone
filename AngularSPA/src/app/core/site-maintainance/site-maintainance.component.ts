import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'core-site-maintainance',
  template: `<title>Site Maintenance</title>
  <body>
    <article>
      <h1>BOSS will be back soon!</h1>
      <div>
        <p>Sorry for the inconvenience but we&rsquo;re performing some maintenance at the moment. If you need to you can
          always <a href="mailto:StaffingSystemSupport.Global@Bain.com">contact us</a>, otherwise we&rsquo;ll be back
          online
          shortly!</p>
        <p>Thanks for staying with us!</p>
      </div>
    </article>
  </body>`,
  styleUrls: ['./site-maintainance.component.scss']
})
export class SiteMaintainanceComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
