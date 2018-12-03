# Changelog for EVE

Find an overview of all new additions.

## EVE1.1

### Energyscape integration

New features for the energyscape experiments

- New Question Type: VisualStimuli
- New Choice question option: Choice with image

### Menu Overhaul

The menu is standardised to make expansion and use more simple

- The loader is renamed launcher (to match with the LaunchManager)
- All menus are now moved to the launcher
  - The evaluation scene is deleted and contents moved to the launcher
  - The questionnaire scene is deleted and contents are moved to the launcher
- All menus are instantiated instead of kept in the background
- All function used within one menu panel are now on the Menu Game Object
- The database can be reseted from the menu

### LaunchManager Overhaul

- The Awake function is cleaned up so that now the first database connection does not require a restart of EVE.
- Add OSC to enable high quality audio with external audio system.

### Community Request

- Add custom images option for scale questions
  - See Experiment/Resources/QuestionSets/TestSet.xml for details
