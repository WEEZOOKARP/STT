import { Stack } from "expo-router";

export default function RootLayout() {
  return (
    <Stack
      screenOptions={{
        headerShown: true,
      }}
    >
      <Stack.Screen
        name="index"
        options={{ title: "Home", headerTitle: "Home" }}
      />
      <Stack.Screen
        name="about"
        options={{ title: "About", headerTitle: "About" }}
      />
    </Stack>
  );
}
