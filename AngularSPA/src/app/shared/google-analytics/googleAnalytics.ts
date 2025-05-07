declare var TrackPageView: any;
declare var TrackEvent: any;
 
export class GoogleAnalytics
{
  public static staffingTrackPageView(ecode: string, pagename: string, parentPageName: string) {
    try {
      TrackPageView(ecode, pagename, parentPageName);
    }
    catch (e) {
      console.log('TrackPageView wasn\'t called. Exception: ' + e.message);
    }
  }

  public static staffingTrackEvent(collection: string, action: string, value: string) {
    try {
      TrackEvent(collection, action, value);
    }
    catch (e) {
      console.log("TrackEvent wasn\'t called. Exception: " + e.message);
    }
  }
}
