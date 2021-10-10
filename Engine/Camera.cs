using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FPS_Engine
{
    // Creates a GameComponent I can use for the camera.
    public class Camera : GameComponent
    {
        // Camera Attributes
        private Vector3 cameraPosition;
        private Vector3 cameraRotation;
        private float cameraSpeed;
        private Vector3 cameraLookAt;
        private Vector3 mouseRotationBuffer;
        private MouseState currentMouseState;
        private MouseState prevMouseState;

        // Key Attributes
        private bool upReleased = true;
        private bool downReleased = true;

        // Camera Properties
        public Vector3 Position
        {
            get { return cameraPosition; }
            set { cameraPosition = value; UpdateLookAt(); }
        }

        public Vector3 Rotation
        {
            get { return cameraRotation; }
            set { cameraRotation = value; UpdateLookAt(); }
        }

        public Matrix Projection
        {
            get;
            protected set;
        }

        public Matrix View
        {
            get
            {
                return Matrix.CreateLookAt(cameraPosition, cameraLookAt, Vector3.Up);
            }
        }

        // Constructor
        public Camera(Game game, Vector3 position, Vector3 rotation, float speed)
            : base(game)
        {
            cameraSpeed = speed;

            // Setup projection matrix
            Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4,                             //            FOV : Basically 45°
                Game.GraphicsDevice.Viewport.AspectRatio,       //   Aspect Ratio : Game's Aspect Ratio
                0.05f,                                          //  Near Clipping : Super bmall so you can see things up 
                1000f                                           //   Far Clipping : Super big so things don't pop in super close
                );

            // Set camera position and rotation
            MoveTo(position, rotation);

            prevMouseState = Mouse.GetState();
        }

        // Set position and rotation of the camera
        private void MoveTo(Vector3 pos, Vector3 rot)
        {
            Position = pos;
            Rotation = rot;
        }

        // Update the camera's target vector
        private void UpdateLookAt()
        {
            // Create rotation matrix based off the camera's pitch and yaw
            Matrix rotationMatrix = Matrix.CreateRotationX(cameraRotation.X) * Matrix.CreateRotationY(cameraRotation.Y);

            // Create target offset vector
            Vector3 lookAtOffset = Vector3.Transform(Vector3.UnitZ, rotationMatrix);

            // Update the camera's target vector
            cameraLookAt = cameraPosition + lookAtOffset;
        }

        // Simulates movement
        private Vector3 PreviewMove(Vector3 amount)
        {
            // Create a rotation matrix
            Matrix rotate = Matrix.CreateRotationY(cameraRotation.Y);

            // Create a movement vector
            Vector3 movement = new Vector3(amount.X, amount.Y, amount.Z);
            movement = Vector3.Transform(movement, rotate);

            // Return the value of the camera's position + movement vector
            return cameraPosition + movement;
        }

        // Actually moves the camera
        private void Move(Vector3 scale)
        {
            MoveTo(PreviewMove(scale), Rotation);
        }

        // Update method
        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds; // Mouse Smoothing

            currentMouseState = Mouse.GetState();

            KeyboardState ks = Keyboard.GetState();

            // Handle basic movement
            Vector3 moveVector = Vector3.Zero;

            if (ks.IsKeyDown(Keys.W)) { moveVector.Z = 1; }
            if (ks.IsKeyDown(Keys.S)) { moveVector.Z = -1; }
            if (ks.IsKeyDown(Keys.A)) { moveVector.X = 1; }
            if (ks.IsKeyDown(Keys.D)) { moveVector.X = -1; }
            
            if (ks.IsKeyDown(Keys.Up)) { Settings.mouseSens += 0.5f; }
            if (ks.IsKeyDown(Keys.Down) && Settings.mouseSens > 0.5f) { Settings.mouseSens -= 0.5f; }

            if (moveVector != Vector3.Zero)
            {
                // Normalize moveVector
                moveVector.Normalize();
                // Add in smooth and speed
                moveVector *= dt * cameraSpeed;

                // Move Camera
                Move(moveVector);
            }

            // Handle mouse movement
            float deltaX;
            float deltaY;

            if(currentMouseState != prevMouseState)
            {
                // Cache the mouse's location
                deltaX = currentMouseState.X - (Game.GraphicsDevice.Viewport.Width / 2);
                deltaY = currentMouseState.Y - (Game.GraphicsDevice.Viewport.Height / 2);

                mouseRotationBuffer.X -= ((0.01f * deltaX) * dt) * Settings.mouseSens;   // Remove "* dt" to
                mouseRotationBuffer.Y -= ((0.01f * deltaY) * dt) * Settings.mouseSens;   // Remove smoothing

                if(mouseRotationBuffer.Y < MathHelper.ToRadians(-75.0f))
                    mouseRotationBuffer.Y -= mouseRotationBuffer.Y - MathHelper.ToRadians(-75.0f); // Prevent the camera from doing flips
                if(mouseRotationBuffer.Y > MathHelper.ToRadians(75.0f))
                    mouseRotationBuffer.Y -= mouseRotationBuffer.Y - MathHelper.ToRadians(75.0f); // Prevent the camera from doing flips

                // Actually rotate the camera
                Rotation = new Vector3(-MathHelper.Clamp(mouseRotationBuffer.Y, MathHelper.ToRadians(-75.0f), MathHelper.ToRadians(75.0f)),
                    MathHelper.WrapAngle(mouseRotationBuffer.X), 0
                    );

                Console.WriteLine("X: {0}\nY: {1}", deltaX, deltaY);

                deltaX = 0;
                deltaY = 0;
            }

            Mouse.SetPosition(Game.GraphicsDevice.Viewport.Width/2, Game.GraphicsDevice.Viewport.Height/2);

            prevMouseState = currentMouseState;

            base.Update(gameTime);
        }
    }
}