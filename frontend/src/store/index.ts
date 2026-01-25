import { configureStore } from '@reduxjs/toolkit';
import projectsReducer from './projectsSlice';
import entriesReducer from './entriesSlice';
import uiReducer from './uiSlice';

export const store = configureStore({
  reducer: {
    projects: projectsReducer,
    entries: entriesReducer,
    ui: uiReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
