# Changelog for EVE

Find an overview of all new additions.

## EVE1.2

### Energyscape & Globalscape integration

New features for the energyscape experiments

- New Question Type: VisualStimuli
- New Choice question option: Choice with image
- New Scale question option: dis/enable labels on toggle buttons

### Menu Overhaul

The menu is standardised to make expansion and use more simple

- The loader is renamed launcher (to match with the LaunchManager)
- All menus are now moved to the launcher
  - The evaluation scene is deleted and contents moved to the launcher
  - The questionnaire scene is deleted and contents are moved to the launcher
- All menus are instantiated instead of kept in the background
- The questionnaire manager no longer manages menus but defers everything to the MenuManager.
- All function used within one menu panel are now on the Menu Game Object
- The database can be reseted from the menu
- Experiments can be started from the parameter session screen (given that a participant id is assigned)

### Questionnaire Overhaul

- Simplify push to database
- Cleanup and standardise of Question base class

### LaunchManager Overhaul

- The Awake function is cleaned up so that now the first database connection does not require a restart of EVE.
- Add OSC to enable high quality audio with external audio system.

### General Overhaul

- Add more documentation
- Follow Reshaper recommendations on code design

### Community Request

- Add custom images option for scale questions
  - See Experiment/Resources/QuestionSets/TestSet.xml for details
