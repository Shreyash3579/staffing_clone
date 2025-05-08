import { Injectable, OnDestroy } from '@angular/core';
import { CoreService } from '../core.service';
import {initialize, LDClient, LDFlagSet, LDUser} from "launchdarkly-js-client-sdk";

@Injectable()
export class LaunchDarklyService implements OnDestroy {
  launchDarklyClient: LDClient;
  isClientInitialized: boolean = false;
  private allFeatureFlags: LDFlagSet;
  constructor(private coreService: CoreService) { 
    
    this.initializeLaunchdarkly();

  }

  initializeLaunchdarkly(){
    this.isClientInitialized = false;
    var user : LDUser = this.getUserObject();
    this.launchDarklyClient = initialize(this.coreService.appSettings.launchDarklyClientId, user);

    this.launchDarklyClient.on('ready', () => {
      this.isClientInitialized = true;
      this.setAllFeatureFlags();
    });

    this.launchDarklyClient.on('change', () => {
      this.setAllFeatureFlags();
    });

  }

  trackUser(){
    //This function is just used when we want to ONLY track the logged in user in launch darkly.
    //Calling this function will initialize the constuctor first which does the tracking work.
    //DO NOT USE this when we want to get feature flag data from launch darkly.
  }

  waitForLDClientInitialization(): Promise<any> {
    return this.launchDarklyClient.waitUntilReady() ;
  }

  getUserObject() : LDUser{
    return  {
      "key": this.coreService.loggedInUser.employeeCode,
      "name": this.coreService.loggedInUser.fullName,
      "firstName": this.coreService.loggedInUser.firstName,
      "lastName": this.coreService.loggedInUser.lastName,
      "email": this.coreService.loggedInUser.internetAddress,
      "custom": {
        "level" : this.coreService.loggedInUser.levelName
      }
    };
  }

  setAllFeatureFlags(){
    this.allFeatureFlags = this.launchDarklyClient.allFlags();
  }

  getFeatureFlagVariation(flagName){
    if(this.allFeatureFlags)
      return this.allFeatureFlags[flagName];
    else
      true;
  }

  ngOnDestroy(): void {
      this.isClientInitialized = false;
      this.launchDarklyClient.close();
  }

}
